#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngan.Dict.Server 雲服務器部署][
本文記錄 `Ngan.Dict.Server` Web 後端以單一 Native AOT 鏡像部署到雲服務器的流程。
]

#H[1. 目標][
- 只使用一個鏡像: `ngan.dict-server:aot-slim`
- 不在服務器編譯，僅在本地構建並上傳鏡像 tar 包
- 同一鏡像支持兩種用法
  - 不傳 `migrate` 時，按原邏輯啓動 Web 服務
  - 傳 `migrate` 時，只執行遷移，結束後退出
- 保持鏡像精簡
  - Native AOT
  - 不打包 `wwwroot`
  - 刪除 `.dbg`、`.pdb`
  - 運行層基於 `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled`
]

#H[2. 關鍵文件][
- `Ngan.Dict.Server/Dockerfile-Aot`
- `Ngan.Dict.Server/Deploy/Cloud/docker-compose.aot-slim.yml`
- `Ngan.Dict.Server/ExternalRsrc/Ngan.Dict.Server.cloud.docker.jsonc`
]

#H[3. 本地構建鏡像][
在 Windows PowerShell 中通過 WSL 執行:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "cd /mnt/e/_code/CsNgan.Dict && docker build -t ngan.dict-server:aot-slim -f ./Ngan.Dict.Server/Dockerfile-Aot ."
```

說明:
- `docker build` 的上下文必須是倉庫根 `E:/_code/CsNgan.Dict`
- `Ngan.Dict.Server.Http` 依賴 `Ngan.Dict.Core`、`Ngan.Dict.Backend`、`Tsinswreng.*` 多個相鄰項目
]

#H[4. 導出鏡像][
```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "docker save ngan.dict-server:aot-slim | gzip -1 > /tmp/ngan.dict-server-aot-slim.tar.gz"
```
]

#H[5. 上傳到服務器][
先把鏡像與部署文件上傳到服務器:

```bash
# 鏡像
scp /tmp/ngan.dict-server-aot-slim.tar.gz root@<server>:/opt/ngan.dict-server/upload/
# docker compose配置
scp Ngan.Dict.Server/Deploy/Cloud/docker-compose.aot-slim.yml root@<server>:/opt/ngan.dict-server/upload/
# 業務配置文件
scp Ngan.Dict.Server/ExternalRsrc/Ngan.Dict.Server.cloud.docker.jsonc root@<server>:/opt/ngan.dict-server/upload/
```
]

#H[6. 服務器部署][
在服務器上執行:

```bash
mkdir -p /opt/ngan.dict-server/deploy /opt/ngan.dict-server/config
mv /opt/ngan.dict-server/upload/docker-compose.aot-slim.yml /opt/ngan.dict-server/deploy/
mv /opt/ngan.dict-server/upload/Ngan.Dict.Server.cloud.docker.jsonc /opt/ngan.dict-server/config/
docker load -i /opt/ngan.dict-server/upload/ngan.dict-server-aot-slim.tar.gz

cd /opt/ngan.dict-server/deploy
docker compose -f docker-compose.aot-slim.yml up -d postgres redis
docker compose -f docker-compose.aot-slim.yml run --rm web migrate Ngan.Dict.Server.cloud.docker.jsonc
docker compose -f docker-compose.aot-slim.yml up -d web
```

第二條命令的解釋:
````md

run：启动一个新容器来执行指定的命令，而不是使用 up 常驻运行的容器。

--rm：命令执行完毕后自动删除该临时容器（不留残骸）。

web：指定 Compose 中定义的 web 服务（作为运行命令的模板，会使用它的镜像、环境变量等）。

migrate：命令行傳參
````
]

#H[7. migrate 參數行爲][
同一鏡像的入口程序爲 `./Ngan.Dict.Server.Http`。

正常啓動:
```bash
./Ngan.Dict.Server.Http Ngan.Dict.Server.cloud.docker.jsonc
```

只跑遷移:
```bash
./Ngan.Dict.Server.Http migrate Ngan.Dict.Server.cloud.docker.jsonc
```

其約定爲:
- 第一個參數是 `migrate` 時，程序執行遷移並退出
- 否則第一個參數視爲配置文件路徑，按原來邏輯啓動 Web
]

#H[8. 驗證][
可用以下命令檢查:

```bash
docker ps
curl -i http://127.0.0.1:2341/Open/Time
```

補充:
- 由於鏡像內已去掉 `wwwroot`，根路徑是否返回前端頁面不再作爲驗證條件
- 以後端 API 可訪問爲準
]
