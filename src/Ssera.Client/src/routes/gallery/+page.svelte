<script lang="ts">
	import { ImageApiHelper } from "$lib/ImageApiHelper";
	import { onMount } from "svelte";
	import type { Entry } from "./GalleryView.svelte";
	import GalleryView from "./GalleryView.svelte";
	import Filters from "./Filters.svelte";

    let loading = $state(true);
    let entries: Entry[] = $state([]);

    type Filters = {
        tags: string[];
        tagSearch: string;
        eras: ImageApiHelper.Era[];
        members: ImageApiHelper.GroupMember[];
        orderBy: ImageApiHelper.OrderByType;
        sortBy: ImageApiHelper.SortType;
        pageSize: number;
        page: number;
    }
    
    let filters: Filters = $state({
        tags: [],
        tagSearch: "",
        eras: [],
        members: [],

        orderBy: 0,
        sortBy: 0,
        pageSize: 0,
        page: 1
    });

    const fetchEntries = async () => {
        loading = true;
        const response = await fetch(ImageApiHelper.ApiRoute + "?page=1&pageSize=50");

        if (!response.ok) {
            console.error(await response.json());
            loading = false;
            return;
        }
        
        entries = (await response.json() as ImageApiHelper.GetResponse).results
            .map(res => {
                let tags = res.tags;
                if (res.era) tags = [ImageApiHelper.EraToHuman(res.era), ...tags];
                return { id: res.id, tags };
            })

        loading = false;
    }

    onMount(() => fetchEntries());

    const applyNewFilters = () => {
        if (!loading) {
            fetchEntries();
        }
    }
</script>

<div id="page">
    <!-- nope, no fucking spread here -->
    <Filters
        tags={filters.tags}
        tagSearch={filters.tagSearch}
        eras={filters.eras}
        members={filters.members}
        orderBy={filters.orderBy}
        sortBy={filters.sortBy}
        pageSize={filters.pageSize}
        onSubmit={applyNewFilters} />

    {#if loading}
    <p>Loading...</p>
    {:else}
    <GalleryView entries={entries} />
    {/if}
</div>

<style>
    #page {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }
</style>