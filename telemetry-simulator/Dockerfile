# Use the official Go image for building
FROM golang:1.21-alpine AS build

# Set working directory
WORKDIR /app

# Copy go mod file first
COPY go.mod ./

# Download dependencies and generate go.sum
RUN go mod download && go mod tidy

# Copy source code
COPY . .

# Build the application
RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o main .

# Use a minimal image for running
FROM alpine:latest

# Install ca-certificates for HTTPS requests
RUN apk --no-cache add ca-certificates

WORKDIR /root/

# Copy the binary
COPY --from=build /app/main .

# Expose port (not needed for this service, but good practice)
EXPOSE 8080

# Run the application
CMD ["./main"]