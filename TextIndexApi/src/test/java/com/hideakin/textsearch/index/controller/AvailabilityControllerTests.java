package com.hideakin.textsearch.index.controller;

import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.content;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.service.PreferenceService;

@SpringBootTest
public class AvailabilityControllerTests {

	private MockMvc mockMvc;

	@Mock
	private PreferenceService preferenceService;

	@InjectMocks
	private AvailabilityController availabilityController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(availabilityController).build();
	}

	@Test
	public void getStatus_successful() throws Exception {
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/health/status"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("OK"));
	}

}
