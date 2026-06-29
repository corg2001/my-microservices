# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Custom image with JAAS configuration for SASL_SSL auth.
# ============================================================
FROM quay.io/debezium/connect:2.6

# Rename original entrypoint so we can wrap it
RUN mv /docker-entrypoint.sh /debezium-entrypoint.sh

# Copy our custom startup script
COPY start-debezium.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 8083

ENTRYPOINT ["/docker-entrypoint.sh"]