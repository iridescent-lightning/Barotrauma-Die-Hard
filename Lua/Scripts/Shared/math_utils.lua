local utils = {}

utils.clamp = function(value, min, max)
    return math.min(math.max(value, min), max);
end

utils.calculateTotalPressure = function (hull, gas)
    return (gas.GetTotalMoles(hull) * 813 * gas.GetTemperature(hull)) / hull.Volume
end

utils.calculatePressure = function (hull, gas, gasname)
    return (gas.GetGas(hull, gasname) * 813 * gas.GetTemperature(hull)) / hull.Volume
end


return utils