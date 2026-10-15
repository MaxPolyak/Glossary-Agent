export interface Term {
  name: string
  definition: string
}

// POST /api/extract отвечает 200 без тела или 500 с текстом ошибки
export async function extractTerms(text: string): Promise<void> {
  const response = await fetch('/api/extract', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `Сервис ответил ${response.status}`)
  }
}

export async function fetchGlossary(): Promise<Term[]> {
  const response = await fetch('/api/glossary')

  if (!response.ok) {
    throw new Error(`Не удалось загрузить глоссарий: ${response.status}`)
  }

  return response.json()
}

// Удаляет все термины, возвращает сколько удалено
export async function clearGlossary(): Promise<number> {
  const response = await fetch('/api/glossary', { method: 'DELETE' })

  if (!response.ok) {
    throw new Error(`Не удалось очистить глоссарий: ${response.status}`)
  }

  const result: { deleted: number } = await response.json()
  return result.deleted
}
