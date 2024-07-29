---@class ConfigDataManager
ConfigDataManager = {}

function ConfigDataManager.Get(configName,id,fieldName)
    local config = LuaCallCS.LoadConfigData(configName,id)

    if config then
        if fieldName then
            return config[fieldName]
        end

        return config
    end

    return nil
end