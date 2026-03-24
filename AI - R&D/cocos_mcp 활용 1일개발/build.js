/**
 * Cocos Creator build automation script
 * Usage: node build.js [platform] [--release] [--deploy <path>] [--reload-ext]
 *
 * Examples:
 *   node build.js web-desktop
 *   node build.js web-desktop --release
 *   node build.js web-desktop --deploy C:/nginx/html/game
 *   node build.js --reload-ext        (reload extension only, no build)
 */
const http = require('http');

const MCP_PORT = 3001;
const RELOAD_ONLY = process.argv.includes('--reload-ext');
const PLATFORM  = (!RELOAD_ONLY && process.argv[2] && !process.argv[2].startsWith('--'))
    ? process.argv[2] : 'web-desktop';
const IS_DEBUG  = !process.argv.includes('--release');
const DEPLOY_IDX = process.argv.indexOf('--deploy');
const DEPLOY_PATH = DEPLOY_IDX >= 0 ? process.argv[DEPLOY_IDX + 1] : null;

// ── helpers ──────────────────────────────────────────────────────────────────

function mcpCall(toolName, toolArgs = {}) {
    return new Promise((resolve, reject) => {
        const body = JSON.stringify({
            jsonrpc: '2.0',
            id: Date.now(),
            method: 'tools/call',
            params: { name: toolName, arguments: toolArgs },
        });
        const req = http.request(
            { hostname: 'localhost', port: MCP_PORT, path: '/mcp', method: 'POST',
              headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(body) } },
            (res) => {
                let data = '';
                res.on('data', c => data += c);
                res.on('end', () => {
                    try {
                        const resp = JSON.parse(data);
                        if (resp.error) return reject(new Error(resp.error.message));
                        const text = resp.result?.content?.[0]?.text;
                        resolve(text ? JSON.parse(text) : resp.result);
                    } catch (e) { reject(e); }
                });
            }
        );
        req.on('error', reject);
        req.write(body);
        req.end();
    });
}

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

function httpPost(path, body = '') {
    return new Promise((resolve, reject) => {
        const req = http.request(
            { hostname: 'localhost', port: MCP_PORT, path, method: 'POST',
              headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(body) } },
            (res) => { let d = ''; res.on('data', c => d += c); res.on('end', () => resolve(d)); }
        );
        req.on('error', reject);
        if (body) req.write(body);
        req.end();
    });
}

async function reloadExtension() {
    console.log('→ Reloading MCP extension...');
    try {
        await httpPost('/reload', '{}');
        console.log('  Reload triggered, waiting 3s for restart...');
        await sleep(3000);
        // Wait for server to come back
        for (let i = 0; i < 10; i++) {
            try {
                const health = await new Promise((res, rej) => {
                    http.get(`http://localhost:${MCP_PORT}/health`, r => {
                        let d = ''; r.on('data', c => d += c);
                        r.on('end', () => res(JSON.parse(d)));
                    }).on('error', rej);
                });
                console.log(`✓ Extension reloaded (${health.tools} tools)`);
                return;
            } catch (e) { await sleep(1000); }
        }
        console.warn('⚠ Server did not come back after reload');
    } catch (e) {
        console.warn('⚠ /reload not available yet (need manual reload first):', e.message);
    }
}

function copyDir(src, dest) {
    const fs   = require('fs');
    const path = require('path');
    if (!fs.existsSync(dest)) fs.mkdirSync(dest, { recursive: true });
    for (const entry of fs.readdirSync(src, { withFileTypes: true })) {
        const s = path.join(src, entry.name);
        const d = path.join(dest, entry.name);
        if (entry.isDirectory()) copyDir(s, d);
        else fs.copyFileSync(s, d);
    }
}

// ── main ─────────────────────────────────────────────────────────────────────

async function main() {
    console.log(`\n=== Cocos Creator Build Automation ===`);
    if (!RELOAD_ONLY) {
        console.log(`Platform : ${PLATFORM}`);
        console.log(`Mode     : ${IS_DEBUG ? 'debug' : 'release'}`);
        if (DEPLOY_PATH) console.log(`Deploy to: ${DEPLOY_PATH}`);
    }
    console.log('');

    // 1. Check MCP server is alive
    try {
        const health = await new Promise((res, rej) => {
            http.get(`http://localhost:${MCP_PORT}/health`, (r) => {
                let d = ''; r.on('data', c => d += c);
                r.on('end', () => res(JSON.parse(d)));
            }).on('error', rej);
        });
        console.log(`✓ MCP server online (${health.tools} tools)`);
    } catch (e) {
        console.error('✗ MCP server not reachable. Is Cocos Creator running with the MCP extension?');
        process.exit(1);
    }

    if (RELOAD_ONLY) { await reloadExtension(); return; }

    // 2. Check builder is ready
    const status = await mcpCall('project_check_builder_status');
    if (!status.data?.ready) {
        console.error('✗ Builder not ready:', JSON.stringify(status));
        process.exit(1);
    }
    console.log('✓ Builder ready');

    // 3. Start build
    console.log(`\n→ Starting build for ${PLATFORM}...`);
    const buildResult = await mcpCall('project_build_project', { platform: PLATFORM, debug: IS_DEBUG });
    console.log('Build response:', JSON.stringify(buildResult, null, 2));

    if (!buildResult.success) {
        console.error('✗ Failed to start build:', buildResult.error || buildResult.message);
        process.exit(1);
    }

    // 4. If build started asynchronously, poll for completion
    console.log('\n→ Polling build status...');
    let attempts = 0;
    const MAX_WAIT = 180; // seconds
    while (attempts < MAX_WAIT) {
        await sleep(2000);
        attempts += 2;
        let info;
        try {
            info = await mcpCall('project_query_build_status');
        } catch (e) {
            console.log(`  [${attempts}s] query failed: ${e.message}`);
            continue;
        }
        if (!info.success) { console.log(`  [${attempts}s] ${info.error}`); continue; }

        const tasks = info.data?.list || [];
        const running = info.data?.queue || {};
        const isFree  = info.data?.free;

        process.stdout.write(`  [${attempts}s] tasks=${tasks.length} free=${isFree}\r`);

        if (isFree && tasks.length > 0) {
            const latest = tasks[tasks.length - 1];
            console.log(`\n✓ Build completed! State: ${latest.state}`);
            console.log(`  Output: ${latest.options?.buildPath || 'unknown'}`);

            if (latest.state === 'success' || latest.state === 'finish') {
                if (DEPLOY_PATH) {
                    const src = `C:/Work/cocos/cocosMCP/build/${PLATFORM}`;
                    console.log(`\n→ Deploying ${src} → ${DEPLOY_PATH}`);
                    const fs = require('fs');
                    if (!fs.existsSync(src)) {
                        console.error(`✗ Build output not found at: ${src}`);
                        process.exit(1);
                    }
                    copyDir(src, DEPLOY_PATH);
                    console.log('✓ Deploy complete!');
                }
                process.exit(0);
            } else {
                console.error(`✗ Build failed with state: ${latest.state}`);
                process.exit(1);
            }
        }
    }
    console.log(`\n✗ Build timed out after ${MAX_WAIT}s`);
    process.exit(1);
}

main().catch(e => { console.error('Error:', e.message); process.exit(1); });
