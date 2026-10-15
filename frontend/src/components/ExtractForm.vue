<script setup lang="ts">
import { ref } from 'vue'
import { samples } from '../samples'

defineProps<{
  loading: boolean
}>()

const emit = defineEmits<{
  submit: [text: string]
}>()

const text = ref('')
const selectedSample = ref<string | null>(null)

const sampleNames = samples.map((sample) => sample.name)

function applySample(name: string | null) {
  const sample = samples.find((item) => item.name === name)
  if (sample) {
    text.value = sample.text
  }
}

function onSubmit() {
  emit('submit', text.value)
}
</script>

<template>
  <v-card>
    <v-card-title>Текст документа</v-card-title>

    <v-card-text>
      <v-form @submit.prevent="onSubmit">
        <v-select
          v-model="selectedSample"
          :items="sampleNames"
          label="Подставить пример"
          density="comfortable"
          variant="outlined"
          clearable
          :disabled="loading"
          @update:model-value="applySample"
        />

        <v-textarea
          v-model="text"
          label="Текст письма, регламента или протокола"
          variant="outlined"
          rows="14"
          counter
          :disabled="loading"
        />

        <div class="d-flex justify-end">
          <v-btn type="submit" color="primary" prepend-icon="mdi-text-search" :loading="loading">
            Извлечь термины
          </v-btn>
        </div>
      </v-form>
    </v-card-text>
  </v-card>
</template>
