# Client App

This is the React frontend for the library and exam portal.

## Local dev setup

```powershell
cd .\client
npm install
npm run dev -- --host 0.0.0.0 --port 5173
```

The app connects to:
- Backend: `http://localhost:5121`
- Frontend: `http://localhost:5173`

## Deployment setup

The frontend reads the backend URL from `VITE_API_BASE_URL`. Copy `.env.example` to `.env.local` for local overrides, or configure the same variable in the hosting provider.

For Vercel, set the project root to `client/`. The included `vercel.json` configures the Vite build, `dist` output, and SPA fallback routes. Deploy the ASP.NET API separately, then set `VITE_API_BASE_URL` to its public URL and configure the API CORS policy to allow the deployed frontend origin.

## Default admin login
- Email: `admin@library.edu`
- Password: `Admin@123`

## Build check

```powershell
cd .\client
npm run build
```

This project includes:
- authentication UI
- library book management
- exam scheduling and seat allocation views
- live notifications via SignalR
- recommendation panel
