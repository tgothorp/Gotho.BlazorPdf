"""Local smoke-test server; run from the solution root, then open
http://127.0.0.1:5098/Gotho.BlazorPdf/Tests/touch-smoke.html.
"""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer


class Handler(SimpleHTTPRequestHandler):
    extensions_map = {
        **SimpleHTTPRequestHandler.extensions_map,
        ".mjs": "text/javascript",
        ".js": "text/javascript",
        ".css": "text/css",
    }

    def log_message(self, format, *args):
        # Keep the server usable if the background runner closes its output pipe.
        pass


ThreadingHTTPServer(("127.0.0.1", 5098), Handler).serve_forever()
