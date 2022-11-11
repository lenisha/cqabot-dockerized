#!/bin/bash
## USAGE  ./builddeploy.sh <ACR_NAME> <Static IP for DirectLine>
set -x #echo on

ACR_NAME=$1
STATIC_IP=$2


echo "----- Deploy Bot"
sed -e "s~__ACR_NAME__~${ACR_NAME}~g" bot-app-acr.yaml > bot-app-deploy.yaml 
kubectl apply -f bot-app-deploy.yaml


echo "---- Deploy Directline Connector"
pushd directline-offline

kubectl apply -f directline-offline-svc.yaml
sleep 10

IP=$(kubectl get service/direct-offline-svc -n bots -o jsonpath='{.status.loadBalancer.ingress[0].ip}')


sed -e "s~__ACR_NAME__~${ACR_NAME}~g" direct-offline-app-acr.yaml > direct-offline-app-deploy.yaml
sed -i "s/__IP__/${IP}/g" direct-offline-app-deploy.yaml

kubectl apply -f direct-offline-app-deploy.yaml

popd

echo "----- Deploy WebChat"
pushd webchat
sed -i "s~__IP__~${STATIC_IP}~g" index.html 

az acr login -n $ACR_NAME
az acr build --image webchat --registry $ACR_NAME .


sed -e "s~__ACR_NAME__~${ACR_NAME}~g" webchat-app-acr.yaml > webchat-app-deploy.yaml 

kubectl apply -f  webchat-app-deploy.yaml 



