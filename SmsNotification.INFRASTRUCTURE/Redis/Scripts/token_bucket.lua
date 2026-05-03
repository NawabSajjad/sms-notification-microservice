local key = KEYS[1]
local capacity = tonumber(ARGV[1])
local refillRate = tonumber(ARGV[2])
local now = tonumber(ARGV[3])

local data = redis.call('HMGET', key, 'tokens', 'lastRefill')

local tokens = tonumber(data[1]) or capacity
local lastRefill = tonumber(data[2]) or now

local delta = now - lastRefill
if delta < 0 then delta = 0 end

local refill = (delta / 1000) * refillRate
tokens = tokens + refill

if tokens > capacity then
    tokens = capacity
end

if tokens < 1 then
    return 0
end

tokens = tokens - 1

redis.call('HSET', key, 'tokens', tokens, 'lastRefill', now)

local ttl = math.ceil(capacity / refillRate)
if ttl < 1 then ttl = 1 end
redis.call('EXPIRE', key, ttl)
return 1