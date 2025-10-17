from transformers import AutoTokenizer, AutoModel
import torch
import numpy as np
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

MODEL_NAME = "allenai/longformer-base-4096"
tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
model = AutoModel.from_pretrained(MODEL_NAME)

class EmbeddingRequest(BaseModel):
    text: str

@app.post("/api/embedding")
def generate_embedding(req: EmbeddingRequest):
    text = req.text
    inputs = tokenizer(text, return_tensors="pt", truncation=True, max_length=4096)
    with torch.no_grad():
        outputs = model(**inputs)
    embedding = outputs.last_hidden_state.mean(dim=1).squeeze().numpy()
    return {"embedding": embedding.tolist()}
