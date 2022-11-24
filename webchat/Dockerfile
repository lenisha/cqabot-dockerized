FROM nginx:1.23.2-alpine


## add permissions for nginx user
#

RUN mkdir -p /var/cache/nginx
RUN chown -R nginx:nginx /var/cache/nginx

RUN  touch /tmp/nginx.pid && \
     chown -R nginx:nginx /tmp/nginx.pid
USER nginx        


COPY --chown=nginx:nginx ./nginx.conf /etc/nginx/nginx.conf
#content, comment out the ones you dont need!
COPY --chown=nginx:nginx ./*.html /usr/share/nginx/html/



EXPOSE 8080
