# Linux Installation

This document describes how to install `ACAAD` on a Linux system.

## Prerequisites

`ACAAD` requires the .NET runtime to be installed on your system. You can download the .NET runtime from
the [official website](https://dotnet.microsoft.com/download).
It provides a script to automatically install the
runtime [here](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install).

## SystemD Service

The software comes with a SystemD host wrapper which automatically interfaces with linux packages like `systemctl` and
`journalctl`. First we create a Systemd unit file:

```sh
touch /etc/systemd/system/acaad-user.service
```

Then we edit the file with the following content:

```
[Unit]
Description=acaad-user

[Service]
Type=notify
ExecStart=/usr/sbin/acaad-user/Oma.WndwCtrl.MgmtApi

[Install]
WantedBy=multi-user.target[MgmtApiService.config.json](../../../../Service.Cfg/User.Linux/MgmtApiService.config.json)
```

The unit file points to the `/user/sbin/acaad-user` location where the software is expected to be installed. Of course
this can be adjusted to your needs. In any case, make sure the software is executable and owned by the user you want to
run it as:

``` sh
sudo chmod +x /usr/sbin/acaad-user/Oma.WndwCtrl.MgmtApi
sudo chown <YOUR_USER> /usr/sbin/acaad-user/*
sudo chmod -R 700 /usr/sbin/acaad-user/
sudo chmod 400 /usr/sbin/acaad-user/component-configuration-linux.json
```

__Note:__ The above setup already hints at something I realized is quite neat during development: You can deploy `ACAAD`
twice, one instance with user permissions and one with admin privileges and configure the admin instance in a way to
automate deployment of the user service. A respective configuration can be found in the `Configuration` C# project.

Now we can enable and start the service:

```sh
sudo systemctl daemon-reload
sudo systemctl start acaad-user
```

If everything works as expected, enable the service so that it auto-starts after a machine reboot:

``` sh
sudo systemctl enable acaad-user
```

## Securing the Service

__TODO...__

## Usual Deployment Flow

1. Build the software
2. Stop the linux service:
    ```sh
    sudo systemctl stop acaad-user
    ```
3. Copy the build output to the installation directory:
    ```sh
    sudo cp -R /mnt/tmpstorage/Build/MgmtApi/Linux/* /usr/sbin/acaad-user/
    ```
   Note: In my setup the build output folder is mounted to a shared NAS folder `/mnt/tmpstorage/Build/MgmtApi/Linux/`.
4. Copy over (overwrite) the configuration files for correct host and port binidings:
    ```sh
    sudo cp /mnt/mapped/Service.Cfg/User.Linux/* /usr/sbin/acaad-user/
    ```
5. Start the service again
    ```sh
    sudo systemctl start acaad-user
    ```

## Further Reading

- [Running .NET Core Applications as a Linux Service](https://code-maze.com/aspnetcore-running-applications-as-linux-service/)
  by [Muhammed Saleem](https://code-maze.com/author/muhammed-saleem/)