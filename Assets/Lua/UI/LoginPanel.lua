---@class LoginPanel
local LoginPanel = PrefabClass("LoginPanel")

function LoginPanel.Awake(instance)
    local gameObject = instance.gameObject

    LuaCallCS.SetActive(gameObject, "Text_Des", false)
    LuaCallCS.SetActive(gameObject, "Text_Progress", false)
    LuaCallCS.SetActive(gameObject, "Sli_Progress", false)

    LuaCallCS.SetSpriteImage(gameObject, "Btn_Login", "Atlas01/01_btn_Cheng2", true)

    LuaCallCS.PlayAnimation(gameObject, nil, "Play", WrapMode.Once, function ()
        LuaCallCS.SetText(gameObject, "Btn_Login/Text", "登录")
        LuaCallCS.AddClickListener(gameObject, "Btn_Login", function()
            SdkMsgManager.Instance:LoginQQ()
        end)
    end)
end

return LoginPanel