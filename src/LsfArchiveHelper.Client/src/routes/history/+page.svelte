<script lang="ts">
	import { onMount } from "svelte";
	import { HistoryApiHelper } from "$lib/HistoryApiHelper";
	
	export const ssr = false;

	let stream: Promise<HistoryApiHelper.ApiResponse> = new Promise(() => {});

	function fetchData() {
		stream = fetch(HistoryApiHelper.ApiRoute).then(response => response.json());
	}

	onMount(() => fetchData());
</script>

<svelte:head>
	<title>History</title>
</svelte:head>

<p>
	The worker runs data import and aggregation on a schedule
</p>

<article id="results-container">
	<h3>Results</h3>
	{#await stream}
		<p>Loading...</p>
	{:then events}
		<table>
			<thead>
			<tr>
				<th scope="col">
					<span>Date</span>
				</th>
				<th scope="col">
					<span>Event count</span>
				</th>
				<th scope="col">
					<span>Time taken</span>
				</th>
				<th scope="col">
					<span>Message</span>
				</th>
			</tr>
			</thead>
			<tbody>
			{#each events as event}
				<tr>
					<td class="col-date">
						<span>{new Date(event.date).toLocaleString()}</span>
					</td>
					<td class="col-eventcount">
						<span>{event.totalEvents}</span>
					</td>
					<td class="col-timetaken">
						<span>{event.timeTaken}</span>
					</td>
					<td class="col-message">
						<span>{event.message ?? ""}</span>
					</td>
				</tr>
			{/each}
			</tbody>
		</table>
	{:catch error}
		<p>Error loading results: {error.message}</p>
	{/await}
</article>


<style>
	#results-container {
		display: flex;
		flex-direction: column;
		gap: 0.75em;
	}

	#results-container {
		& table {
			padding: 6px;
			border: 1px solid black;
			border-collapse: collapse;
		}

		& tr {
			border-top: 1px solid gray;
			background-color: rgb(250, 250, 250);

			&:nth-child(2n) {
				background-color: rgb(242, 242, 242);
			}
		}

		& :is(th, td) {
			padding: 4px;

			&:not(:first-child) {
				border-left: 1px solid gray;
			}

			& > :is(a, span) {
				display: flex;
				justify-content: flex-start;
				align-items: center;

				max-width: 80ch;
			}

			&.col-link > :is(a, span) {
				word-break: break-all;
			}
		}
	}

</style>
