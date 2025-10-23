from fastapi import FastAPI, Request
from sentence_transformers import SentenceTransformer
import uvicorn
import asyncio
from concurrent.futures import ThreadPoolExecutor

app = FastAPI()

# Load the model
model = SentenceTransformer("all-MiniLM-L6-v2")

# Thread pool to avoid blocking
executor = ThreadPoolExecutor(max_workers=2)

async def encode_text(text: str):
    loop = asyncio.get_running_loop()
    return await loop.run_in_executor(executor, model.encode, text)

@app.post("/api/embedding")
async def generate_embedding(request: Request):
    data = await request.json()
    text = data.get("text", "")
    if not text.strip():
        return {"error": "Text is required"}

    embedding = await encode_text(text)
    return {"embedding": embedding.tolist()}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=5001)
