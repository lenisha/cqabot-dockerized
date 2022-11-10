#!/bin/bash
## USAGE  ./builddeploy.sh <RG NAME> <APPSERVICE NAME>
set -x #echo on

ACR_NAME=$1

az acr login -n $ACR_NAME

echo "Build Bot Container"
az acr build --image cqabot --registry $ACR_NAME .

echo "Build Directline Offline Container"
pushd directline-offline
az acr build --image direct-offline --registry $ACR_NAME .
popd

echo "Build WebChat Sample Container"
pushd webchat
az acr build --image webchat --registry $ACR_NAME .
popd
   

   