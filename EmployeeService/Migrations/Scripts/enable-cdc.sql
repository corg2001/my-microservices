-- ============================================================
-- enable-cdc.sql
-- Enables Change Data Capture on the Employees table.
-- Run once against my-microservices-db.
-- Safe to run again — checks prevent duplicate enablement.
-- ============================================================

-- Step 1: Enable CDC on the database (if not already enabled)
IF NOT EXISTS (
    SELECT 1 FROM sys.databases
    WHERE name = DB_NAME() AND is_cdc_enabled = 1
)
BEGIN
    EXEC sys.sp_cdc_enable_db;
    PRINT 'CDC enabled on database: ' + DB_NAME();
END
ELSE
BEGIN
    PRINT 'CDC already enabled on database: ' + DB_NAME();
END
GO

-- Step 2: Enable CDC on the Employees table (if not already enabled)
IF NOT EXISTS (
    SELECT 1 FROM cdc.change_tables
    WHERE source_object_id = OBJECT_ID('dbo.Employees')
)
BEGIN
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'dbo',
        @source_name   = N'Employees',
        @role_name     = NULL,           -- No gating role — service identity controls access
        @supports_net_changes = 1;       -- Enables net change queries (latest state per row)
    PRINT 'CDC enabled on table: dbo.Employees';
END
ELSE
BEGIN
    PRINT 'CDC already enabled on table: dbo.Employees';
END
GO

-- Step 3: Verify
SELECT
    OBJECT_SCHEMA_NAME(t.source_object_id) AS source_schema,
    OBJECT_NAME(t.source_object_id)        AS source_table,
    t.capture_instance,
    t.supports_net_changes,
    t.start_lsn
FROM cdc.change_tables t
WHERE t.source_object_id = OBJECT_ID('dbo.Employees');
GO