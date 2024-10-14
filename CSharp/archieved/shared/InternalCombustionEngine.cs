using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Items.Components;
using Barotrauma;



namespace BarotraumaDieHard
{
    public partial class InternalCombustionEngine : Engine
    {
       
        public static float escapedTime = 0f;
        public static float updateInterval = 0.1f;

        public float lubricant;
        public float lubricantMax = 1000f;
        public float ammonia;
        public float ammoniaMax = 1000f;
        public float temperature;
        public bool useElectric = false;
        public bool breakTransmission = false;
        public float requiredTurbineRate;
        public float turbineRate;
        public float maxPowerOutput;
        public float currForce;
        
        [Editable, Serialize(0f, IsPropertySaveable.Yes, description: "How much lubericant does this engine have.", alwaysUseInstanceValues: true)]
        public float Lubricant
        {
            get { return lubricant; }
            set { lubricant = MathHelper.Clamp(value, 0f, lubricantMax); }
        }
        [Editable, Serialize(0f, IsPropertySaveable.Yes, description: "How much lubericant does this engine have.", alwaysUseInstanceValues: true)]
        public float Ammonia
        {
            get { return ammonia; }
            set { ammonia = MathHelper.Clamp(value, 0f, ammoniaMax); }
        }
        [Editable, Serialize(3000f, IsPropertySaveable.Yes, description: "How much power can this engine provide.", alwaysUseInstanceValues: true)]
        public float MaxPowerOutput
        {
            get { return maxPowerOutput; }
            set { maxPowerOutput = value; }
        }

        

        public float Temperature
        {
            get { return temperature; }
            set 
            { 
                if (Lubricant < 25f)
                {
                    temperature = MathHelper.Clamp(value, 0f, 1000f); 
                }
                else
                {
                    temperature = MathHelper.Clamp(value, 0f, 700f);
                }
                
            }
        }

        public InternalCombustionEngine(Item item, ContentXElement element)
            : base(item, element)
        {
            IsActive = true;
            InitProjSpecificDieHard(element);
        }

        partial void InitProjSpecificDieHard(ContentXElement element);
        // This method must be included or desync.
        partial void UpdateAnimation(float deltaTime);

        public override void Update(float deltaTime, Camera cam)
        {
            

            UpdateOnActiveEffects(deltaTime);

            UpdateAnimation(deltaTime);
            if (Ammonia <= 0f)
            {
                turbineRate = 0f;
            }
            Temperature += 0.1f * turbineRate * deltaTime;
            Temperature -= 10f * deltaTime;
            Lubricant -= 0.01f * turbineRate * deltaTime;
            Ammonia -= 0.01f * turbineRate * deltaTime;


            controlLockTimer -= deltaTime;

            if (powerConsumption == 0.0f || !useElectric)
            {
                prevVoltage = 1;
                hasPower = true;
            }
            else
            {
                hasPower = Voltage > MinVoltage;
            }

            if (lastReceivedTargetForce.HasValue)
            {
                targetForce = lastReceivedTargetForce.Value;
            }
            Force = MathHelper.Lerp(force, (Voltage < MinVoltage) ? 0.0f : targetForce, deltaTime * 10.0f);
            if (Math.Abs(Force) > 1.0f)
            {
                float voltageFactor = MinVoltage <= 0.0f ? 1.0f : Math.Min(Voltage, MaxOverVoltageFactor);
                currForce = force * MathF.Pow(voltageFactor, PowerToForceExponent);
                float condition = item.MaxCondition <= 0.0f ? 0.0f : item.Condition / item.MaxCondition;
                // Broken engine makes more noise.
                float noise = Math.Abs(currForce) * MathHelper.Lerp(1.5f, 1f, condition);
                UpdateAITargets(noise);
                //arbitrary multiplier that was added to changes in submarine mass without having to readjust all engines
                float forceMultiplier = 0.1f;
                if (User != null)
                {
                    forceMultiplier *= MathHelper.Lerp(0.5f, 2.0f, (float)Math.Sqrt(User.GetSkillLevel("helm") / 100));
                }
                currForce *= item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.EngineMaxSpeed, MaxForce) * forceMultiplier;
                if (item.GetComponent<Repairable>() is { IsTinkering: true } repairable)
                {
                    currForce *= 1f + repairable.TinkeringStrength * TinkeringForceIncrease;
                }

                currForce = item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.EngineSpeed, currForce);

                //less effective when in a bad condition
                currForce *= MathHelper.Lerp(0.5f, 2.0f, condition);

                requiredTurbineRate = MathHelper.Lerp(0f, 100f, Math.Abs(targetForce / 100f));
                //DebugConsole.NewMessage("Required Turbine Rate: " + targetForce , Color.Red);
                // Update the current force based on fuel
                if (turbineRate <= requiredTurbineRate)
                {
                    //DebugConsole.NewMessage("Turbine rate is less than required turbine rate", Color.Red);
                    currForce = 0f;
                }
                if (item.Submarine.FlippedX) { currForce *= -1; }
                Vector2 forceVector = new Vector2(currForce, 0);
                item.Submarine.ApplyForce(forceVector * deltaTime * Timing.FixedUpdateRate);
                UpdatePropellerDamage(deltaTime);
                
                // Update the temperature
                if (Temperature >= 800f)
                {
                    item.Condition -= 1f * deltaTime;
                }
                
                
                
                
                DebugConsole.NewMessage("Force Ratio: " + force);
                DebugConsole.NewMessage("Target Force Ratio: " + targetForce);// ranged from -100 to 100

                DebugConsole.NewMessage("current Force: " + currForce);
                DebugConsole.NewMessage("Max Force: " + maxForce);//maxForce is the default property for vanilla game.
                DebugConsole.NewMessage("Force: " + Force);
                

#if CLIENT
                float particleInterval = 1.0f / particlesPerSec;
                particleTimer += deltaTime;
                while (particleTimer > particleInterval)
                {
                    Vector2 particleVel = -forceVector.ClampLength(5000.0f) / 5.0f;
                    GameMain.ParticleManager.CreateParticle("bubbles", item.WorldPosition + PropellerPos * item.Scale,
                        particleVel * Rand.Range(0.8f, 1.1f),
                        0.0f, item.CurrentHull);
                    particleTimer -= particleInterval;
                }
#endif
            
            }
        }

        public override float GetCurrentPowerConsumption(Connection connection = null)
        {
            if (!useElectric && turbineRate >= requiredTurbineRate)
            {
                return -MathHelper.Lerp(0, maxPowerOutput, turbineRate - requiredTurbineRate) / 100f;
            }
            else if (connection != this.powerIn || !IsActive || !useElectric)
            {
                return 0;
            }
            

            currPowerConsumption = MathF.Pow(Math.Abs(targetForce) / 100.0f, ForceToPowerExponent) * powerConsumption;
            //engines consume more power when in a bad condition
            item.GetComponent<Repairable>()?.AdjustPowerConsumption(ref currPowerConsumption);
            return currPowerConsumption;
        }
        
        public override PowerRange MinMaxPowerOut(Connection connection, float load = 0) 
        {
            if (connection == powerOut) 
            {
                
                float minOut = maxPowerOutput;// * (1 - PowerTolerance);
                float maxOut = maxPowerOutput;// * (1 + PowerTolerance);
                //DebugConsole.NewMessage(PowerConsumption.ToString());
                return new PowerRange(minOut, maxOut, MaxPowerOutput);
            }
            return PowerRange.Zero;
        }

        public override float GetConnectionPowerOut(Connection connection, float power, PowerRange minMaxPower, float load) 
        {
            
            if (connection == powerOut) 
            {
                
                if (powerOut.Grid != null && powerOut.Grid.Load != null)
                {
                    float turbineExcess = MathHelper.Clamp(turbineRate - requiredTurbineRate, 0f, 200f);
                    DebugConsole.NewMessage("Turbine Excess: " + turbineExcess);
                    float powerOutPut = MathHelper.Lerp(0, maxPowerOutput, turbineExcess / 200f);
                    float tolerance = (powerOutPut - powerOut.Grid.Load) * 0.5f;
                    return powerOutPut - tolerance;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
            return 0;
            }
        }


        public override XElement Save(XElement parentElement)
        {
            Vector2 prevPropellerPos = PropellerPos;
            //undo flipping before saving
            if (item.FlippedX) { PropellerPos = new Vector2(-PropellerPos.X, PropellerPos.Y); }
            if (item.FlippedY) { PropellerPos = new Vector2(PropellerPos.X, -PropellerPos.Y); }
            XElement element = base.Save(parentElement);
            PropellerPos = prevPropellerPos;
            // Add the lubricant information to the saved element
            element.Add(new XElement("Lubricant", Lubricant));
            element.Add(new XElement("Ammonia", Ammonia));
            return element;
        }

        public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap)
        {
            base.Load(componentElement, usePrefabValues, idRemap);

            // Load lubricant data from the XML
            Lubricant = componentElement.GetAttributeFloat("Lubricant", componentElement.Parent.GetAttributeFloat("Lubricant", Lubricant));
            Ammonia = componentElement.GetAttributeFloat("Ammonia", componentElement.Parent.GetAttributeFloat("Ammonia", Ammonia));
        }

        
    }
}
