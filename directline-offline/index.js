const directline = require("offline-directline");
const express = require("express");

const app = express();
const bot_url = process.env.BOT_URL ? process.env.BOT_URL: "http://127.0.0.1:3978/api/messages";
directline.initializeRoutes(app, 3000 , bot_url);