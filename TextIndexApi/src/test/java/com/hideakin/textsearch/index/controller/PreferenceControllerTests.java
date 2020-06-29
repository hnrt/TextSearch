package com.hideakin.textsearch.index.controller;

import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.hideakin.textsearch.index.model.PreferenceRequest;
import com.hideakin.textsearch.index.service.PreferenceService;

@SpringBootTest
public class PreferenceControllerTests {

	private MockMvc mockMvc;

	@Mock
	private PreferenceService preferenceService;

	@InjectMocks
	private PreferenceController preferenceController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(preferenceController).build();
	}

	@Test
	public void getPreference_successful() throws Exception {
		when(preferenceService.getPreference("foo")).thenReturn("bar");
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/preferences/foo"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$.value").value("bar"));
	}

	@Test
	public void getPreference_notFound() throws Exception {
		when(preferenceService.getPreference("foo")).thenReturn(null);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/preferences/foo"))
        	.andExpect(status().isNotFound());
	}

	@Test
	public void createPreference_successful() throws Exception {
		doNothing().when(preferenceService).createPreference(argThat(x -> x.equals("quux")), argThat(x -> x.equals("fred")));
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new PreferenceRequest("quux", "fred"));
		mockMvc.perform(MockMvcRequestBuilders
				.post("/v1/preferences")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isCreated());
		verify(preferenceService, times(1)).createPreference("quux", "fred");
	}

	@Test
	public void updatePreference_successful() throws Exception {
		when(preferenceService.updatePreference("quux", "fred")).thenReturn(true);
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new PreferenceRequest("quux", "fred"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/preferences")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isOk());
		verify(preferenceService, times(1)).updatePreference("quux", "fred");
	}

	@Test
	public void updatePreference_notFound() throws Exception {
		when(preferenceService.updatePreference("quux", "fred")).thenReturn(false);
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(new PreferenceRequest("quux", "fred"));
		mockMvc.perform(MockMvcRequestBuilders
				.put("/v1/preferences")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isNotFound());
		verify(preferenceService, times(1)).updatePreference("quux", "fred");
	}
	
	@Test
	public void deletePreference_successful() throws Exception {
		when(preferenceService.deletePreference("thud")).thenReturn(true);
		mockMvc.perform(MockMvcRequestBuilders.delete("/v1/preferences/thud"))
			.andExpect(status().isOk());
		verify(preferenceService, times(1)).deletePreference("thud");
	}

	@Test
	public void deletePreference_notFound() throws Exception {
		when(preferenceService.deletePreference("thud")).thenReturn(false);
		mockMvc.perform(MockMvcRequestBuilders.delete("/v1/preferences/thud"))
			.andExpect(status().isNotFound());
		verify(preferenceService, times(1)).deletePreference("thud");
	}

}
