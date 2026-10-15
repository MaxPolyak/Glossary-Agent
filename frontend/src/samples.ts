// Те же тексты, которыми бэкенд проверяется скриптом backend/tests/run-samples.ps1.
// Vite подставит содержимое файлов как строки.
const files = import.meta.glob<string>('../../backend/tests/samples/*.txt', {
  query: '?raw',
  import: 'default',
  eager: true,
})

export interface Sample {
  name: string
  text: string
}

export const samples: Sample[] = Object.entries(files)
  .map(([path, text]) => ({
    name: path.split('/').pop()!.replace('.txt', ''),
    text: text.trim(),
  }))
  .sort((a, b) => a.name.localeCompare(b.name))
