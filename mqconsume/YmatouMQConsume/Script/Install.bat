echo �밴�������ʼ��װ�ͻ�����ƽ̨�ĺ�̨����
echo.
pause
echo.
echo ����ԭ�з� ����
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /U ../YmatouMQConsume.exe >InstallService.log 
echo.
echo ������ϣ���ʼ��װ��̨����
echo.
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe ../YmatouMQConsume.exe >InstallService.log
echo ����װ��ϣ���������
net start YmatouMqConsumeService >InstallService.log 
echo.
echo �������������� InstallService.log �в鿴����Ĳ�������� 
echo.
pause