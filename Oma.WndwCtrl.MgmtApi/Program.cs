using Oma.WndwCtrl.MgmtApi;

MgmtApiService apiService = new(null);
await apiService.StartAsync();
await apiService.WaitForShutdownAsync();