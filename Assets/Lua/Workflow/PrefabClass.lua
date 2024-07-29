---@class PrefabClass
PrefabClass = {}

local class = {}

function class:Init(name)
    self.name = name

    rawset(self,"Close",function ()
        LuaCallCS.ClosePrefabPanel(self.name)
    end)
end

function class:Hello()
    print("Hello你的" .. self.name)
end

local metatable = {
    __call = function(prefabClass,name,parentTable)
        parentTable = parentTable or {}

        parentTable.__index = class

        local table = setmetatable({}, parentTable)

        table:Init(name)

        return table
    end
}

setmetatable(PrefabClass, metatable)