@echo Off

"%MsBuildExe%" MyGetBuild.proj /p:Configuration="%Configuration%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false