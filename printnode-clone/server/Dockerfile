FROM node:18-alpine

WORKDIR /app

# Copia package.json
COPY package*.json ./

# Installa dipendenze
RUN npm ci --only=production

# Copia il resto dei file
COPY . .

# Esponi porta
EXPOSE 3000

# Crea directory per database e logs
RUN mkdir -p /app/logs

# Avvia il server
CMD ["node", "server.js"]
