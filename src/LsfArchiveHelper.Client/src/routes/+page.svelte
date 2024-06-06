<svelte:head>
	<title>Home</title>
</svelte:head>

<h1>Le Sserafim archive helper</h1>
<p>
	Aggregates data from the Le Sserafim archive spreadsheet and enables easy navigation, filtering and sorting.
</p>
<section>
	<h3>Links</h3>
	<p>
		Le Sserafim archive spreadsheet:
		<a href="https://docs.google.com/spreadsheets/d/14NeZu0cI5Tkd8jkGuORRdukFtAtGPnNFUSzVoLZnZuc">
			Google sheets
		</a>
	</p>
	<p>
		Source: <a href="https://github.com/svee4/LsfArchiveHelper">Github</a>
	</p>
	<p>
		Report problems: <a href="https://github.com/svee4/LsfArchiveHelper/issues/new">Github issue</a>
	</p>
	<p>
		Contact discord: <span style="">@alexnj</span>
	</p>
</section>

<!-- make this visible when its relevant -->
<article style="display: flex; flex-direction: column; gap: 1em; display: none; ">
	<h3>Change log</h3>
	<article>
		<!-- lists suck -->
		<table id="changelog">
			<thead>
			<tr>
				<th scope="col">Date</th>
				<th scope="col">Title</th>
				<th scope="col">Information</th>
			</tr>
			</thead>
			<tbody>
			<tr>
				<td class="col-date">2024-06-06</td>
				<td class="col-title">Initial release</td>
				<td class="col-information">
					<p>
						Blehh
					</p>
				</td>
			</tr>
			</tbody>
		</table>
	</article>
</article>

<style>
	#changelog {
		padding: 6px;
		border: 1px solid black;
		border-collapse: collapse;

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
