echo 请按任意键开始安装客户管理平台的后台服务
echo.
echo 卸载原有服务项
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /U YmatouMQConsume.exe > InstallService.log 
echo 卸载完成
echo.
pause