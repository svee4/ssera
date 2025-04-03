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
        sort: ImageApiHelper.SortType;
        pageSize: number;
        page: number;
    }
    
    let filters: Filters = $state({
        tags: [],
        tagSearch: "",
        eras: [],
        members: [],

        orderBy: 0,
        sort: 0,
        pageSize: 0,
        page: 1
    });

    const fetchEntries = async () => {
        loading = true;

        let q = new URLSearchParams();
        
        for (const tag in filters.tags) {
            q.append("tags", tag);
        }

        if (filters.tagSearch) {
            q.append("tagSearch", filters.tagSearch);
        }

        for (const era of filters.eras) {
            q.append("eras", ImageApiHelper.Era[era]);
        }

        for (const mem of filters.members) {
            q.append("members", ImageApiHelper.GroupMember[mem]);
        }

        q.append("orderBy", ImageApiHelper.OrderByType[filters.orderBy]);
        q.append("sort", ImageApiHelper.SortType[filters.sort]);
        q.append("pageSize", filters.pageSize.toString());
        q.append("page", filters.page.toString());

        const response = await fetch(ImageApiHelper.ApiRoute + "?" + q.toString());

        if (!response.ok) {
            console.error(await response.json());
            loading = false;
            return;
        }
        
        entries = (await response.json() as ImageApiHelper.GetResponse).results
            .map(res => {
                let tags = res.tags;
                if (res.era) tags = [res.era, ...tags];
                return { id: res.id, tags, date: new Date(res.date) };
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
        bind:tags={filters.tags}
        bind:tagSearch={filters.tagSearch}
        bind:eras={filters.eras}
        bind:members={filters.members}
        bind:orderBy={filters.orderBy}
        bind:sort={filters.sort}
        bind:pageSize={filters.pageSize}
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