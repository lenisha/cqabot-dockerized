// Importing process module
const process = require('process');

const directline = require("./dist/bridge");
const express = require("express");

const app = express();
const bot_url = process.env.BOT_URL ? process.env.BOT_URL: "http://127.0.0.1:3978/api/messages";
const directline_domain = process.env.DIRECTLINE_DOMAIN ? process.env.DIRECTLINE_DOMAIN: "127.0.0.1";

directline.initializeRoutes(app, 3000 , bot_url,directline_domain);



process.on('beforeExit', (code) => {
    console.log('Process beforeExit event with code: ', code);
  });
  
  process.on('exit', (code) => {
    console.log('Process exit event with code: ', code);
  });
  