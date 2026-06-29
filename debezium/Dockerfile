# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Base image includes Kafka Connect runtime.
# We add the SQL Server connector plugin on top.
# ============================================================
FROM debezium/connect:2.7

# SQL Server connector is bundled in debezium/connect — no extra install needed.
# This Dockerfile exists so we can version-pin and customize if needed.

# Expose Kafka Connect REST API port
EXPOSE 8083