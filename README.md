# Language Bot fully in AKS

In this example we will demonstrate how to build Bot with Bot composer which will query Language Service Knowledge Base and deploy bot to AKS as docker container

Here is Logical Flow view for all Components
![Bot Logical](docs/Bot_logical.png)

Deployement Architure (on private locked network)
![Bot Physical](docs/Bot_physical.png)



# Build and Deploy everything
Create AKS and ACR and get creadentials from AKS
Prerequisites 

- AZ cli and kubectl
- create AKS and ACR
- Create Language service and project with KB
- Update ConfigMap `bot-app-cm.yaml` with setting for language service `endpointKey`, `hostname`, `projectname`

```sh
az login --tenant <TENANTID>
./buildall.sh <ACRNAME>
./deployall.sh <ACRNAME> <Directline Static IP>
```

**Note** All AKS LoadBalancer services are using private internal IPs as per docs https://learn.microsoft.com/en-us/azure/aks/internal-lb


## Running all Containers in non priviledged mode

All containers are running with non Root user context according to framework
All Kubernetes manifests enable securityContext and read only non priviledged mode

Framework specifics:
- Nginx https://hub.docker.com/_/nginx
- Node https://github.com/nodejs/docker-node/blob/main/16/alpine3.16/Dockerfile 
- Dotnet https://github.com/dotnet/dotnet-docker/issues/1772 

# Create Custom Language answering KB 

## Prerequisites
- This project requires a [Language resource](https://aka.ms/create-language-resource) with Custom question answering enabled.

### Configure knowledge base of the project
- Follow instructions [here][Quickstart] to create a Custom question answering project. You will need this project's name to be used as `ProjectName` in [appsettings.json](appsettings.json).
- Visit [Language Studio][LS] and open created project.
- Go to `Edit knowledge base` -> Click on `...` -> Click on `Import questions and answers` -> Click on `Import as TSV`.
- Import [SampleForCQA.tsv](SampleForCQA.tsv) file.
- You can test your knowledge base by clicking on `Test` option.
- Go to `Deploy knowledge base` and click on `Deploy`.


# Build Bot with Bot Composer

- Install and launch Bot Composer, create Empty bot (C#) template
For this project set Recognizer to Regex - we are not using LU

![Bot recognizer](docs/Bot_recognizer.png)

- Add new Find Answers Intent and Dialog that could collect user question and query KB

![Bot find answers](docs/Bot_findanswers.png)

- Update Greeting dialog to suggest Bot capabilities

![Bot find answers](docs/Bot_suggestanswers.png)

- Update FindAnsers Dialog to get user input and send it to Language service

![Bot input](docs/Bot_input.png)

user input is stored  in `user.question` variable on user scope

![Bot input](docs/Bot_sendrestcall.png)

REST call is sent to language `hostname` and `projectname` and use `endpointKey` for the service obtained from Azure Language Service.This settings need to be configured in Bot settings Json.

![Bot settings](docs/Bot_settings.png)


- Test Bot in Test Emulator
![Bot settings](docs/Bot_test.png)

# Deploy Bot
Composer generates ASP.NET Core based code that could be run in Docker on AKS

## Set Language Service settings
Secrets are not stored in repo to add Language service `endpointKey` add `settings/appsettings.Production.json` file with key for CQA endpoint:

```json
{
  "qna": {
    "endpointKey": "xxxxx"
  }
}
```

# Build Bot Container

```
docker  build -t cqabot .
docker tag cqabot <ACR>.azurecr.io/cqabot

az login --tenant <TENANTID>
az acr login  --name acrforbots
docker push <ACR>.azurecr.io/cqabot
```

## Test Bot Loacally

```sh
docker run -it --rm -p 3978:3978  --env ASPNETCORE_URLS=http://+:3978  --name cqa_sample cqabot
```

Run Non Priviledged Non root locally

```sh
 docker run -it --rm -p 3978:3978  --env ASPNETCORE_URLS=http://+:3978  --name cqa_sample   --user 1000:3000  --cap-drop ALL --read-only  --mount type=tmpfs,tmpfs-size=100M,destination=/tmp   cqabot
 ```

# Deploy Bot to AKS
Create ACR and Kubernetes

- Update ConfigMap `bot-app-cm.yaml` with setting for language service `endpointKey`, `hostname`, `projectname`
- Replace in `bot-app-acr.yaml` ACRName to the registry name you have careted

```sh
kubectl create ns bots
kubectl apply -f bot-app-cm.yaml
kubectl apply -f bot-app-acr.yaml
```

Check EXTERNAL-IP for the LB service in front of Bot pods
```
$ kubectl get svc -A
NAME                 TYPE           CLUSTER-IP     EXTERNAL-IP    PORT(S)          AGE
cqabot-svc           LoadBalancer   10.0.151.170   10.240.0.4     3978:31055/TCP   16m
direct-offline-svc   LoadBalancer   10.0.196.102   10.240.2.255   3000:31669/TCP   16m
webchat-app-svc      LoadBalancer   10.0.249.201   10.240.0.5     8080:30912/TCP   15m
```

# Connect Bot Emulator to Remote AKS Bot 

- Make sure to install ngrok version 2 (no support for latest version 3 yet) https://dl.equinox.io/ngrok/ngrok/stable/archive

- Install Bot Emulator

- connect Emulator to Remote Bot add URL (as per wiki https://github.com/microsoft/BotFramework-Emulator/wiki/Getting-Started#connecting-to-bots-hosted-remotely)

![Bot settings](docs/Bot_remoteemulator.png)

# Build and Deploy Direct offline container

Same stepts to build DirectOffline container and deploy to AKS

```
docker  build -t direct-offline .
docker tag cqabot <ACR>.azurecr.io/direct-offline

az login --tenant <TENANTID>
az acr login  --name acrforbots
docker push <ACR>.azurecr.io/direct-offline
```

- Run locally
```
docker run -it --rm -p 3000:3000 --name direct_sample direct-offline
```

 Run locally unpriveledged
```sh
 docker run -it --rm -p 3000:3000  --name direct_sample   --user 1000:1000  --cap-drop ALL --read-only  direct-offline
```


## Deploy Directline on AKS

- Choose Static IP from the AKS Subnet range that is not used and add it in `loadBalancerIP` in `directline-offline/direct-offline-svc.yaml` (https://learn.microsoft.com/en-us/azure/aks/internal-lb)

```yaml
---
apiVersion: v1
kind: Service
metadata:
  name: direct-offline-svc
  namespace: bots
  annotations:
    service.beta.kubernetes.io/azure-load-balancer-internal: "true"
spec:
  type: LoadBalancer
  loadBalancerIP: 10.240.2.255 # Static internal IP 
  ports:
  - port: 3000
  selector:
    app: direct-offline-app
```

- Replace in YAML for `directline-offline/direct-offline-app.yaml` IP of the directline service in environment variable `DIRECTLINE_DOMAIN`

```yaml
    spec:
      containers:
      - name: direct-offline-app
        image: acrforbots.azurecr.io/direct-offline:latest
        ports:
        - containerPort: 3000
        env:
        - name: BOT_URL
          value: http://cqabot-svc.bots.svc.cluster.local:3978/api/messages
        - name: DIRECTLINE_DOMAIN
          value: 10.240.2.255
```


```sh
kubectl apply -f directline-offline/direct-offline-app.yaml
```



# Build WebChat app

Replace in  `webchat/index.html` domain with IP for DirectLine Service:

```
const botConnectionSettings = new BotChat.DirectLine({
            secret: '1234',
            token: '123',
            domain: 'http://20.175.199.127:3000/directline',
            webSocket: false // defaults to true
         });
```

- build docker
```
docker  build -t webchat .
docker tag cqabot <ACR>.azurecr.io/webchat

az login --tenant <TENANTID>
az acr login  --name acrforbots
docker push <ACR>.azurecr.io/webchat
```

- Run locally in read unpriveledged mode
```sh
 docker run -it --rm -p 8080:8080  --name wch_sample   --user 101:101  --cap-drop ALL --read-only  --mount type=tmpfs,tmpfs-size=100M,destination=/tmp  -v $(pwd)/nginx-cache:/var/cache/nginx webchat
```

- deploy in AKS

```sh
kubectl apply -f webchat/webchat-deploy.yaml
```

- Access web chat

http://IP_OF_LB:8080/index.html

![Bot settings](docs/Bot_webchat.png)



