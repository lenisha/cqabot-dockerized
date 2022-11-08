
# Create Custom Language answering 

- Login to Language Studio https://language.cognitive.azure.com/
- Create project, Edit Knoledge base, Import 
- 

# Build Bot with Bot Composer

Add settings/appsettings.Production.json with key for CQA endpoint

{
  "qna": {
    "endpointKey": "xxxxx"
  }
}

# Build Bot Container

```
docker  build -t cqabot .
docker tag cqabot acrforbots.azurecr.io/cqabot

az login --tenant xxxx
az acr login  --name acrforbots
docker push acrforbots.azurecr.io/cqabot
```

# Test Bot Loacally

docker run -it --rm -p 8080:8080 --name cqa_sample cqabot


# Connect Bot Emulator to AKS Bot 

- install ngrok version 2 (no support for latest version 3 yet)


# Build Direct offline container

docker run -it --rm -p 8080:8080 --name cqa_sample cqabot


# Build WebChat app