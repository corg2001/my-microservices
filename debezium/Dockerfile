# ============================================================
# Debezium Server — native Azure Event Hubs sink
# ============================================================
FROM quay.io/debezium/server:2.6

USER root

COPY generate-config.py /generate-config.py
RUN python3 /generate-config.py && chmod +x /custom-entrypoint.sh

USER 1001

ENTRYPOINT ["/custom-entrypoint.sh"]