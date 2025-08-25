#Android

#打包设置
CustomBuildScript.cs设置打包细节
签名文件：***.keystore
签名文件的密码：******

#与Assets文件夹在同一目录下的WebData.txt文件自行创建，内容格式如下：
热更新根目录\n
服务器的公网地址:端口号\n
基础验证用户名\n
基础验证密码\n

#然后需要点击BuildWebBinFile按钮导出Web配置文件到StreamingAssets文件夹下才能与服务器交互