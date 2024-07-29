--------------------------------------------------------------------------------
--      Copyright (c) 2015 - 2016 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------
if jit then
	if jit.opt then
		jit.opt.start(3)
	end

	--print("ver" .. jit.version_num .. " jit: ", jit.status())
	--print(string.format("os: %s, arch: %s", jit.os, jit.arch))
end

if DebugServerIp then
	require("mobdebug").start(DebugServerIp)
end

require "ToLua.misc.functions"
Mathf      = require "ToLua.UnityEngine.Mathf"
Vector3    = require "ToLua.UnityEngine.Vector3"
Quaternion = require "ToLua.UnityEngine.Quaternion"
Vector2    = require "ToLua.UnityEngine.Vector2"
Vector4    = require "ToLua.UnityEngine.Vector4"
Color      = require "ToLua.UnityEngine.Color"
Ray        = require "ToLua.UnityEngine.Ray"
Bounds     = require "ToLua.UnityEngine.Bounds"
RaycastHit = require "ToLua.UnityEngine.RaycastHit"
Touch      = require "ToLua.UnityEngine.Touch"
LayerMask  = require "ToLua.UnityEngine.LayerMask"
Plane      = require "ToLua.UnityEngine.Plane"
Time       = reimport "ToLua.UnityEngine.Time"

list       = require "ToLua.list"
utf8       = require "ToLua.misc.utf8"

require "ToLua.event"
require "ToLua.typeof"
require "ToLua.slot"
require "ToLua.System.Timer"
require "ToLua.System.coroutine"
require "ToLua.System.ValueType"
require "ToLua.System.Reflection.BindingFlags"

--require "ToLua.misc.strict"