<template>
	<div>Status: {{ status }}</div>
</template>

<script setup lang="ts">
definePageMeta({
	name: 'status',
	layout: false,
});
</script>

<script lang="ts">
export default {
	data() {
		return {
			status: 'OK',
		};
	},
};
</script>
