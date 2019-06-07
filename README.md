# CharlesProxyImporterForFiddler
Import Charles JSON Session Files directly in Fiddler

Ensuring proper conversion for all request/response types is going to be an imperfect exercise.  Some concessions will be made due to the differences in how Charles and Fiddler handle sessions.

### Build instructions
After building (I use VS2017), the current PostBuildEvent will copy the DLL to %LOCALAPPDATA%\Programs\Fiddler\ImportExport.  JSON.Net's DLL also needs to be copied there as well.

**NOTE** that this will only import the .chlsj JSON session files from Charles Proxy.  This is the format the mobile version of Charles exports to.  There are no plans to support the binary .chls format as it appears to be a binary serialization from the internal Java objects Charles is using.
