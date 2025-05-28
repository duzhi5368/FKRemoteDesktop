TODO:
- 需要更多的shellcode免杀加壳方案
- 客户端需要关闭和移除部分功能以确定到底是哪些部分引起了VT查杀，原本8/72，逐步升级到20/72，甚至最终版的32/72，这个问题很严重。

已知BUG：
- SettingsFromServer 中的 RECONNECTDELAY 写入失败。
- 文件传输和写入部分有问题，总出现读取失败。
- Chrome的v10密码解密出错。

VT查杀情况：
- BasicTemplate.cs  35/72 （无加密）
- SgnBasicTemplate.cs 35/72 （SGN加密）
- FKShellcodeBasicTemplate.cs 20/72（自定义加密）（卡巴斯基，MS,火绒，Symentec）
- acmDriverEnum 19/72 (自定义加密)（Symentec,McAfee）
- XorBasicTemplate 20/72(仅XOR加密) （Symentec，McAfee，卡巴，火绒）
- FindText 17/72(自定义加密)（Symentec，McAfee）


