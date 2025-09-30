## Dev and Production Workflows

### Development
- API: run `AIN.Api` profile `http` (http://localhost:5177).
- Frontend: in this folder run:
  - `npm install`
  - `npm run dev`
- Calls to `/api/*` are proxied to `http://localhost:5177`.

### Production build
- From this folder:
  - `npm install`
  - `npm run build`
- Output goes to `AIN.Api/wwwroot/app`.
- ASP.NET serves static files and SPA fallback at `/app/*` to `app/index.html`.

  # Arabic Admin and Authority Dashboards

  This is a code bundle for Arabic Admin and Authority Dashboards. The original project is available at https://www.figma.com/design/RIzpKCfOATf3oLmLtne6TW/Arabic-Admin-and-Authority-Dashboards.

  ## Running the code

  Run `npm i` to install the dependencies.

  Run `npm run dev` to start the development server.
  