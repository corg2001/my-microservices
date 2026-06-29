# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Base image includes Kafka Connect runtime + SQL Server connector.
# ============================================================
FROM quay.io/debezium/connect:2.6

# SQL Server connector is bundled in debezium/connect — no extra install needed.
# Using quay.io registry (Debezium's official registry since Docker Hub deprecation).

EXPOSE 8083