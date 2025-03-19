import { ApiHelper } from "$lib/ApiHelper";

export namespace HistoryApiHelper {

	export const ApiRoute = `${ApiHelper.ApiDomain}/api/history` as const;

	export type ApiResponse = {
		date: string,
		totalEvents: number,
		timeTaken: string,
		message?: string
	}[];
}
