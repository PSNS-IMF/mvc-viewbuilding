@echo Off

"%MsBuildExe%" Build.proj /p:Configuration="%Configuration%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false