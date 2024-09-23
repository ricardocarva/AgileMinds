FROM mysql:latest

# Add your custom configuration or initialization scripts here
COPY ./init.sql /docker-entrypoint-initdb.d/

EXPOSE 3306