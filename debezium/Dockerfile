# ============================================================
# Debezium Server — native Azure Event Hubs sink
# ============================================================
FROM quay.io/debezium/server:2.6

USER root

COPY custom-entrypoint.sh /custom-entrypoint.sh
RUN chmod +x /custom-entrypoint.sh

USER 1001

ENTRYPOINT ["/custom-entrypoint.sh"]