using Oma.WndwCtrl.MgmtApi;

MgmtApiService apiService = new(messageBusAccessor: null, rootConfiguration: null);
await apiService.StartAsync();
await apiService.WaitForShutdownAsync();