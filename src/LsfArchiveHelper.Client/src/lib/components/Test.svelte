<script lang="ts">
	import { writable } from "svelte/store";

    export let number: number;

    const store = writable(0);
    number = $store;

</script>

<button on:click={() => number++}>
    {number}
</button>