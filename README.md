# Dota Analytics API 

Backend service for professional Dota 2 match analytics with OpenDota API integration.  
Features three-level caching, automated pro-match preloading, and full Docker containerization.

## Features

- **Smart Caching**: Memory Cache → PostgreSQL → OpenDota API fallback
- **Auto Preload**: Automatically loads latest pro matches on startup (with rate limiting)
- **Docker Ready**: Full containerization via Docker Compose (API + PostgreSQL)
- **Robust Validation**: Business rules for duplicate players and account IDs
- **Advanced Filtering**: Pagination, sorting, and filtering by player/account
- **Swagger UI**: Interactive API documentation out of the box

## Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/dota-analytics-api.git
   cd Dota-Analytics****
