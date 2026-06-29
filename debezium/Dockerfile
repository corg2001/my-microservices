# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Custom image with JAAS configuration for SASL_SSL auth.
# ============================================================
FROM quay.io/debezium/connect:2.6

USER root

# Copy original entrypoint to a new name so we can wrap it
RUN cp /docker-entrypoint.sh /debezium-entrypoint.sh

# Copy our custom startup script over the original entrypoint
COPY start-debezium.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh /debezium-entrypoint.sh

USER 1001

EXPOSE 8083

ENTRYPOINT ["/docker-entrypoint.sh"]