import os

entrypoint = r"""#!/bin/bash
set -e

echo "=== Generating Debezium Server config ==="

mkdir -p /debezium/conf
mkdir -p /debezium/data

cat > /debezium/conf/application.properties << PROPS
# Source: SQL Server CDC
debezium.source.connector.class=io.debezium.connector.sqlserver.SqlServerConnector
debezium.source.database.hostname=my-microservices-sql.database.windows.net
debezium.source.database.port=1433
debezium.source.database.user=sqladmin
debezium.source.database.password=${SQL_PASSWORD}
debezium.source.database.names=my-microservices-db
debezium.source.database.encrypt=true
debezium.source.database.trustServerCertificate=false
debezium.source.table.include.list=my-microservices-db.dbo.Employees
debezium.source.topic.prefix=cdc
debezium.source.snapshot.mode=initial
debezium.source.schema.history.internal=io.debezium.storage.file.history.FileSchemaHistory
debezium.source.schema.history.internal.file.filename=/debezium/data/schema-history.dat
debezium.source.offset.storage=org.apache.kafka.connect.storage.FileOffsetBackingStore
debezium.source.offset.storage.file.filename=/debezium/data/offsets.dat
debezium.source.offset.flush.interval.ms=60000
# Sink: Azure Event Hubs native AMQP
debezium.sink.type=eventhubs
debezium.sink.eventhubs.connectionstring=${EVENTHUBS_CONNECTION_STRING}
debezium.sink.eventhubs.hubname=employee-salary-changes
# Format
debezium.format.value=json
debezium.format.key=json
PROPS

echo "=== Config written ==="
echo "=== Starting Debezium Server ==="
exec /debezium/run.sh
"""

with open('/custom-entrypoint.sh', 'w') as f:
    f.write(entrypoint)

os.chmod('/custom-entrypoint.sh', 0o755)
print('Entrypoint written successfully')