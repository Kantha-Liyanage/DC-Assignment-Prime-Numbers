# Start Consul
consul agent -dev

# Nodea
consul members

# Stop Consul
consul leave

# Consul UI
http://localhost:8500/ui/dc1/services

# Register/Update a service
localhost:8500/v1/agent/service/register

# Deregister a service
localhost:8500/v1/agent/service/deregister/<TestAPI>

# Read single service
http://localhost:8500/v1/catalog/service/<TestAPI>

# Query services using service meata
localhost:8500/v1/catalog/node-services/ubuntu?filter=Meta.method==update
